/***
 
.x-attachment-btn {
   border-color: #6262e5 !important;
}

.x-attachment-btn-clear .x-btn-inner {
   color: #f33;
}

 ***/ //Required CSS

//定義FileGridField元件
Ext.define('WEBAPP.form.FileGridField', {
    extend: 'Ext.form.field.Base',
    requires: [
        'WEBAPP.utils.File', //WEBAPP.utils.File = FileUtil
        'WEBAPP.store.FileStore',
        'WEBAPP.form.FileToolbar'],
    alias: 'widget.filegrid',

    /*******************************
    ** 公有屬性，可在config設定
    ********************************/
    canUpload: true, //是否可以上傳檔案
    canDelete: true, //是否可以刪除檔案
    maxFiles: 0, //最多存幾個檔案, 0表示無限制
    autoGenerateUploadKey: true,
    browseButtonText: '瀏覽',
    clearButtonText: '清除',
    uploadButtonText: '上傳',
    deleteButtonText: '刪檔',
    defaultUploadKey: null,
    uploadKey: null,
    readOnly: true,

    /*******************************
    ** 私有屬性，勿設定或使用
    ********************************/
    isUploadKeyConfirmed: true,

    //FileGridField元件初始化
    initComponent: function () {
        var me = this;

        me.buildField();

        //initComponent呼叫這行才能初始化物件
        me.callParent(arguments);
    },

    //@private 建立檔案列表
    buildField: function () {
        var me = this;
        me.fileStore = Ext.create('WEBAPP.store.FileStore', {
            fileField: this,
            listeners: {
                beforeload: function (store, options) {
                    var np = {
                        UK: store.fileField.getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        me.fileToolbar = Ext.create('WEBAPP.form.FileToolbar', {
            parent: me,
            canDelete: me.canDelete,
            canUpload: me.canUpload,
            maxFiles: me.maxFiles,
            browseButtonText: me.browseButtonText,
            clearButtonText: me.clearButtonText,
            uploadButtonText: me.uploadButtonText,
            deleteButtonText: me.deleteButtonText,
            store: me.fileStore
        });

        me.fileGrid = Ext.create('Ext.grid.Panel', {
            parent: me,
            store: me.fileStore,
            dockedItems: [me.fileToolbar],
            viewConfig: {
                //checkcolumn不標示dirty record
                markDirty: false
            },
            hideHeaders: true,
            columns: [
                {
                    xtype: 'checkcolumn',
                    width: 30,
                    dataIndex: 'SL',
                    hidden: true,
                    menuDisabled: true,
                    stopSelection: false,
                    renderer: function (value, meta, record, rowIndex, colIndex, store) {
                        if (record.data.DN) {
                            meta['tdCls'] = '';
                        } else {
                            meta['tdCls'] = 'x-item-disabled';
                        }
                        return new Ext.ux.CheckColumn().renderer(value);
                    },
                    listeners: {
                        checkchange: function (column, recordIndex, checked, record) {
                            var grid = this.up('grid');
                            var toolbar = grid.down('toolbar');
                            var selectedFiles = grid.getStore().queryBy(
                                function (record) {
                                    return record.get('DN') && record.get('SL');
                                });
                            toolbar.getDeleteButton().setDisabled(selectedFiles.length == 0);
                        }
                    }
                },
                {
                    renderer: function (val, meta, record) {
                        var result = '';
                        if (record.get('DN')) {
                            result = '<a href="javascript:FileUtil.DownloadFile({fileGuid:\'' + record.get('FG') + '\'});"> <img src="../../../Images/save.gif" align="absmiddle" /> ' + record.get('FN') + '</a>';
                            if (record.get('ST') != 'Y')
                                result += ' (未儲存)';
                        }
                        if (!record.get('DN')) {
                            result = '<span style="color:red">' + record.get('FN') + '</sapn>';
                            result += ' <span style="color:#000">(未上傳)</span>';
                        }
                        return result;
                    },
                    flex: 1
                }
            ],
            height: this.height || 150,
            width: this.width || 150,
            showMask: function (msg) { this.loadingMask.msg = msg; this.loadingMask.show(); },
            hideMask: function () { this.loadingMask.hide(); }
        });

        me.fileGrid.loadingMask = new Ext.LoadMask(me.fileGrid, { msg: '初始化中...' });

        //me.items = [me.fileGrid];
        me.childComponent = me.fileGrid;
    },


    getSubTplMarkup: function () {
        var buffer = Ext.DomHelper.generateMarkup(this.childComponent.getRenderTree(), []);
        return buffer.join('');
    },

    finishRenderChildren: function () {
        this.callParent(arguments);
        this.childComponent.finishRender();
    },

    // --- Resizing ---

    onResize: function (w, h) {
        this.callParent(arguments);
        this.childComponent.setSize(w - this.labelWidth, h);
    },

    setReadOnly: function (value) {
        var me = this;
        me.readOnly = value;
        
        if (me.readOnly) {
            me.fileToolbar.hide();
            me.fileGrid.columns[0].hide();
        }
        else {
            me.fileToolbar.show();
            me.fileGrid.columns[0].show();
        }
    },

    // --- Value handling ---
    getUploadKeyConfirmed : function() {
        return this.isUploadKeyConfirmed;
    },

    setUploadKeyConfirmed : function(value) {
        if (this.isUploadKeyConfirmed) //如果isUploadKeyConfirmed一旦判定為false，就不能變更
            this.isUploadKeyConfirmed = value;
    },

    getKeyValid: function () {
        //alert('getKeyValid(): uploadKey="' + this.uploadKey + '"');
        if (this.uploadKey == null) return false;
        if (this.uploadKey == '') return false;
        return true;
    },

    tryLoad: function () {
        //alert('tryLoad(): uploadKey="' + this.uploadKey + '"');
        var me = this;
        if (this.getKeyValid()) {
            me.fileStore.load();
        }
        else {
            me.fileStore.removeAll();
            me.fileToolbar.resetButtonStatus();
            if (this.autoGenerateUploadKey) {
                me.fileGrid.showMask('取得上傳識別中...');
                FileUtil.NewGuid({
                    fileField: me,
                    doneCallback: function (args, result) {
                        var fileField = args.fileField;
                        var fileGrid = args.fileField.fileGrid;
                        fileField.defaultUploadKey = result.newGuid;
                        fileGrid.hideMask();
                    }
                });
            }
        }
    },

    setValue: function (value) {
        //alert('setValue(): uploadKey="' + this.uploadKey + '"');
        if (this.uploadKey != value) this.isUploadKeyConfirmed = true;
        this.uploadKey = value;
        this.tryLoad();
    },

    getValue: function () {
        //alert('getValue:uploadKey=' + this.uploadKey);
        return this.uploadKey;
    },

    getSubmitValue: function () {
        if (this.getKeyValid())
            return this.uploadKey;
        return this.defaultUploadKey;
    }
});