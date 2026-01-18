import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { assetFrontendService } from '../Services/assetFrontendService';

const AssetContext = createContext();

export const useAssets = () => {
    const context = useContext(AssetContext);
    if (!context) {
        throw new Error('useAssets must be used within AssetProvider');
    }
    return context;
};

export const AssetProvider = ({ children }) => {
    const [assets, setAssets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastFetchTime, setLastFetchTime] = useState(null);

    // Cache duration: 1 hour (3600000 ms)
    const CACHE_DURATION = 60 * 60 * 1000;

    const fetchAssets = useCallback(async (forceRefresh = false) => {
        // Kiểm tra cache nếu không force refresh
        if (!forceRefresh && lastFetchTime) {
            const timeSinceLastFetch = Date.now() - lastFetchTime;
            if (timeSinceLastFetch < CACHE_DURATION && assets.length > 0) {
                // Cache vẫn còn hiệu lực, không cần fetch lại
                return;
            }
        }

        try {
            setLoading(true);
            setError(null);
            const response = await assetFrontendService.getAllActiveAssets();
            
            if (response.data && response.data.success && response.data.data) {
                setAssets(response.data.data);
                setLastFetchTime(Date.now());
            } else {
                throw new Error('Failed to load assets');
            }
        } catch (err) {
            console.error('Error loading assets:', err);
            setError(err.message || 'Failed to load assets');
        } finally {
            setLoading(false);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [assets.length, lastFetchTime]);

    // Fetch assets khi mount (chỉ 1 lần)
    useEffect(() => {
        fetchAssets();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Empty dependency - chỉ chạy 1 lần khi mount

    // Helper functions
    const getAssetById = (id) => {
        return assets.find(asset => asset.id === id);
    };

    const getAssetsByType = (assetType) => {
        return assets.filter(asset => asset.assetType === assetType);
    };

    const getFirstAssetByType = (assetType) => {
        return assets.find(asset => asset.assetType === assetType);
    };

    const getAssetUrl = (assetType, fallbackUrl = null) => {
        const asset = getFirstAssetByType(assetType);
        return asset?.imageUrl || fallbackUrl;
    };

    // Refresh assets (khi admin update)
    const refreshAssets = useCallback(() => {
        fetchAssets(true);
    }, [fetchAssets]);

    const value = {
        assets,
        loading,
        error,
        refreshAssets,
        
        // Helper functions
        getAssetById,
        getAssetsByType,
        getFirstAssetByType,
        getAssetUrl,
        
        // Shortcuts cho các asset types được quản lý (chỉ Logo, DefaultCourse, DefaultLesson)
        logos: getAssetsByType(1), // AssetType.Logo = 1
        defaultCourses: getAssetsByType(4), // AssetType.DefaultCourse = 4
        defaultLessons: getAssetsByType(5), // AssetType.DefaultLesson = 5
        
        // Quick access functions - chỉ các asset types còn được quản lý
        getLogo: () => getFirstAssetByType(1)?.imageUrl, // AssetType.Logo = 1
        getDefaultCourseImage: () => getFirstAssetByType(4)?.imageUrl, // AssetType.DefaultCourse = 4
        getDefaultLessonImage: () => getFirstAssetByType(5)?.imageUrl, // AssetType.DefaultLesson = 5
    };

    return (
        <AssetContext.Provider value={value}>
            {children}
        </AssetContext.Provider>
    );
};

export default AssetContext;
