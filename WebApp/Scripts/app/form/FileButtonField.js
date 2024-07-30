/***
 
.x-attachment-btn {
   border-color: #6262e5 !important;
}

 ***/ //Required CSS

//定義FileButtonField元件
Ext.define('WEBAPP.form.FileButtonField', {
    extend: 'Ext.form.FieldContainer',
    mixins: { field: 'Ext.form.field.Base' },
    requires: [
        'WEBAPP.utils.File', //WEBAPP.utils.File = FileUtil
        'WEBAPP.store.FileStore',
        'WEBAPP.form.FileToolbar'],
    alias: 'widget.filebutton',

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
    fileQty: 0,
    viewport: null,

    /*******************************
    ** 私有屬性，勿設定或使用
    ********************************/
    isUploadKeyConfirmed: true,

    //FileButtonField元件初始化
    initComponent: function () {
        var me = this;

        me.buildField();

        //initComponent呼叫這行才能初始化物件
        me.callParent(arguments);
    },

    getDefaultConfig: function () {
        return {
        };
    },

    //@private 建立檔案列表
    buildField: function (args) {
        var me = this;
        me.fileStore = Ext.create('WEBAPP.store.FileStore', {
            fileField: me,
            listeners: {
                beforeload: function (store, options) {
                    var np = {
                        UK: store.fileField.getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        me.fileButton = Ext.create('Ext.Button', {
            disabled: true,
            text: me.buttonText + ' (<font color="#f00">' + me.fileQty + '</font>)',
            listeners: {
                click: function (btn) {
                    var me = btn;
                    var fileButtonField = me.parent;
                    if (fileButtonField.fileWindow == null) {
                        fileButtonField.fileToolbar = Ext.create('WEBAPP.form.FileToolbar', {
                            parent: fileButtonField,
                            canDelete: fileButtonField.canDelete,
                            canUpload: fileButtonField.canUpload,
                            maxFiles: fileButtonField.maxFiles,
                            browseButtonText: fileButtonField.browseButtonText,
                            clearButtonText: fileButtonField.clearButtonText,
                            uploadButtonText: fileButtonField.uploadButtonText,
                            deleteButtonText: fileButtonField.deleteButtonText,
                            store: fileButtonField.fileStore
                        });

                        fileButtonField.fileGrid = Ext.create('Ext.grid.Panel', {
                            store: fileButtonField.fileStore,
                            dockedItems: [fileButtonField.fileToolbar],
                            viewConfig: {
                                //checkcolumn不標示dirty record
                                markDirty: false
                            },
                            hideHeaders: false,
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
                                    hidden: !fileButtonField.showPreview,
                                    text: '預覽',
                                    width: 400,
                                    renderer: function (val, meta, record) {
                                        switch (record.get('FT')) {
                                            case 'jpg':
                                            case 'jpeg':
                                            case 'png':
                                            case 'JPG':
                                            case 'JPEG':
                                            case 'PNG':
                                                return '<img src="/api/File/DownloadImage/' + record.get("FG") + '">';
                                            default:
                                                return '無預覽';
                                        }
                                    },
                                    sortable: false
                                },
                                {
                                    text: '下載',
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
                                    sortable: false
                                },
                                {
                                    xtype: 'datecolumn',
                                    text: '上傳日期',
                                    dataIndex: 'FC',
                                    format: 'X/m/d H:i:s',
                                    width: 130
                                },
                                {
                                    text: '上傳人員',
                                    dataIndex: 'UNA'
                                },
                                { sortable: false, flex: 1 }
                            ],
                            height: '100%',
                            width: '100%',
                            buttons: [{
                                text: '返回',
                                handler: function () {
                                    this.up('window').hide();
                                    fileButtonField.setFileQty(fileButtonField.fileStore.getCount());
                                }
                            }],
                            showMask: function (msg) { this.loadingMask.msg = msg; this.loadingMask.show(); },
                            hideMask: function () { this.loadingMask.hide(); }
                        });

                        fileButtonField.fileGrid.loadingMask = new Ext.LoadMask(fileButtonField.fileGrid, { msg: '初始化中...' });

                        fileButtonField.fileWindow = Ext.create('WEBAPP.form.FileWindow', {
                            title: fileButtonField.title,
                            viewport: fileButtonField.getViewport(),
                            items: fileButtonField.fileGrid
                        });

                        fileButtonField.fileWindow.parent = fileButtonField;
                    }

                    if (fileButtonField.readOnly) {
                        fileButtonField.fileToolbar.hide();
                        fileButtonField.fileGrid.columns[0].hide();
                    }
                    else {
                        fileButtonField.fileToolbar.show();
                        fileButtonField.fileGrid.columns[0].show();
                    }

                    fileButtonField.fileWindow.show();
                    /*
                    fileButtonField.fileWindow.show(
                        undefined,
                        function () {
                            this.parent.tryLoad();
                        }
                    );
                    */
                },
                afterrender: function (c) {
                    /*
                    var atmButton = c;
                    var ajaxRequest = $.ajax({
                        type: "POST",
                        url: '/api/UR1016/GetUploadKey',
                        dataType: "json",
                        data: { DK: this.uploadKey }
                    })
                        .done(function (data, textStatus) {
                        })
                        .fail(function (data, textStatus) {
                        });
                    */
                }
            }
        });

        me.fileButton.parent = me;

        me.items = [me.fileButton];
    },

    getViewport: function ()
    {
        if (this.viewport == null) {
            var svp = this.up('viewport');
            if (svp != null)
                return svp;
            return Ext.getBody();
        }
        return this.viewport;
    },

    setViewport: function (viewport) 
    {
        this.viewport = viewport;
    },

    // --- Rendering ---
    setReadOnly: function (value) {
        this.readOnly = value;
    },

    // --- Value handling ---
    getUploadKeyConfirmed() {
        return this.isUploadKeyConfirmed;
    },

    setUploadKeyConfirmed(value) {
        if (this.isUploadKeyConfirmed) //如果isUploadKeyConfirmed一旦判定為false，就不能變更
            this.isUploadKeyConfirmed = value;
    },

    getKeyValid: function () {
        if (this.uploadKey == null) return false;
        if (this.uploadKey == '') return false;
        return true;
    },
    
    tryLoad: function () {
        //alert('tryLoad(): uploadKey="' + this.uploadKey + '"');
        var me = this;
        if (this.getKeyValid()) {
            me.setFileQty(0);
            me.fileStore.load(
                {
                    callback: function () {
                        me.setFileQty(me.fileStore.getCount());
                    }
                });
        }
        else {
            me.fileStore.removeAll();
            me.fileToolbar.resetButtonStatus();
            me.setFileQty(me.fileStore.getCount())
            if (me.fileWindow != null && me.autoGenerateUploadKey) {
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

    setFileQty: function (fileQty) {
        var me = this;
        me.fileQty = fileQty;
        me.fileButton.setText(me.buttonText + ' (<font color="#f00">' + me.fileQty + '</font>)');
        me.fileButton.setDisabled(false);
    },

    setValue: function (value) {
        //alert('setValue:uploadKey=' + value);
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